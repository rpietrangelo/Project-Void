// Eldritch Dominion — PlayFab CloudScript
// Deploy via: PlayFab Dashboard > Automation > CloudScript > Revisions > Upload
// Version: 1.0.0

var BUILDING_COSTS = {
    // Format: "buildingId_targetLevel": { PG: cost, buildSeconds: time }
    "city_hall_order_1":     { PG: 0,    buildSeconds: 0   },
    "city_hall_order_2":     { PG: 500,  buildSeconds: 60  },
    "city_hall_order_3":     { PG: 1500, buildSeconds: 240 },
    "city_hall_order_4":     { PG: 4500, buildSeconds: 960 },
    "city_hall_order_5":     { PG: 13500,buildSeconds: 3840},
    "barracks_order_1":      { PG: 200,  buildSeconds: 30  },
    "barracks_order_2":      { PG: 600,  buildSeconds: 120 },
    "barracks_order_3":      { PG: 1800, buildSeconds: 480 },
    "farm_order_1":          { PG: 150,  buildSeconds: 20  },
    "farm_order_2":          { PG: 450,  buildSeconds: 80  },
    "farm_order_3":          { PG: 1350, buildSeconds: 320 },
    "watchtower_order_1":    { PG: 300,  buildSeconds: 45  },
    "watchtower_order_2":    { PG: 900,  buildSeconds: 180 },
    "sanctum_cult_1":        { PG: 0,    buildSeconds: 0   },
    "sanctum_cult_2":        { PG: 500,  buildSeconds: 60  },
    "ritual_circle_cult_1":  { PG: 200,  buildSeconds: 30  },
    "ritual_circle_cult_2":  { PG: 600,  buildSeconds: 120 },
    "void_font_cult_1":      { PG: 150,  buildSeconds: 20  },
    "void_font_cult_2":      { PG: 450,  buildSeconds: 80  },
    "monster_lair_cult_1":   { PG: 400,  buildSeconds: 60  },
    "monster_lair_cult_2":   { PG: 1200, buildSeconds: 240 }
};

handlers.StartBuildingUpgrade = function(args, context) {
    var playerId   = currentPlayerId;
    var buildingId = args.buildingId;
    var slotIndex  = args.slotIndex;
    var targetLevel = args.targetLevel;
    var costKey = buildingId + "_" + targetLevel;

    log.info("StartBuildingUpgrade: player=" + playerId +
             " building=" + buildingId + " level=" + targetLevel);

    // 1. Validate cost table entry exists
    var costEntry = BUILDING_COSTS[costKey];
    if (!costEntry) {
        return { success: false, error: "Unknown building or level: " + costKey };
    }

    // 2. Get player building state
    var internalData = server.GetUserInternalData({ PlayFabId: playerId, Keys: ["buildings"] });
    var buildings = (internalData.Data["buildings"])
        ? JSON.parse(internalData.Data["buildings"].Value)
        : {};
    var currentBuilding = buildings[slotIndex] || { level: 0, state: "empty" };

    // 3. Validate level progression (no skipping)
    if (targetLevel !== currentBuilding.level + 1) {
        return { success: false, error: "Invalid level jump. Current: " + currentBuilding.level };
    }

    // 4. Validate not already upgrading
    if (currentBuilding.state === "upgrading") {
        return { success: false, error: "Already upgrading slot " + slotIndex };
    }

    // 5. Validate and deduct currency
    if (costEntry.PG && costEntry.PG > 0) {
        try {
            server.SubtractUserVirtualCurrency({
                PlayFabId: playerId,
                VirtualCurrency: "PG",
                Amount: costEntry.PG
            });
        } catch (e) {
            return { success: false, error: "Insufficient Pale Gold" };
        }
    }

    // 6. Record upgrade start
    var finishTime = Date.now() + (costEntry.buildSeconds * 1000);
    buildings[slotIndex] = {
        buildingId:        buildingId,
        level:             currentBuilding.level,
        state:             "upgrading",
        targetLevel:       targetLevel,
        upgradeFinishTime: finishTime
    };

    server.UpdateUserInternalData({
        PlayFabId: playerId,
        Data: { buildings: JSON.stringify(buildings) }
    });

    log.info("StartBuildingUpgrade: success, finishTime=" + finishTime);
    return { success: true, finishTime: finishTime, costPG: costEntry.PG };
};

handlers.CompleteUpgrade = function(args, context) {
    var playerId  = currentPlayerId;
    var slotIndex = args.slotIndex;
    var now       = Date.now();

    var internalData = server.GetUserInternalData({ PlayFabId: playerId, Keys: ["buildings"] });
    if (!internalData.Data["buildings"]) {
        return { success: false, error: "No building data found" };
    }

    var buildings = JSON.parse(internalData.Data["buildings"].Value || "{}");
    var building  = buildings[slotIndex];

    if (!building || building.state !== "upgrading") {
        return { success: false, error: "No upgrade in progress at slot " + slotIndex };
    }

    // Server-side timer validation — cannot skip
    if (now < building.upgradeFinishTime) {
        return {
            success: false,
            error: "Timer not complete",
            remainingMs: building.upgradeFinishTime - now
        };
    }

    // Complete the upgrade
    building.level       = building.targetLevel;
    building.state       = "ready";
    delete building.upgradeFinishTime;
    delete building.targetLevel;
    buildings[slotIndex] = building;

    server.UpdateUserInternalData({
        PlayFabId: playerId,
        Data: { buildings: JSON.stringify(buildings) }
    });

    log.info("CompleteUpgrade: slot=" + slotIndex +
             " buildingId=" + building.buildingId + " newLevel=" + building.level);

    return { success: true, buildingId: building.buildingId, newLevel: building.level };
};

handlers.GetPlayerBuildings = function(args, context) {
    var internalData = server.GetUserInternalData({
        PlayFabId: currentPlayerId,
        Keys: ["buildings"]
    });
    var buildings = internalData.Data["buildings"]
        ? JSON.parse(internalData.Data["buildings"].Value)
        : {};
    return { success: true, buildings: buildings };
};

handlers.OpenChest = function(args, context) {
    var playerId   = currentPlayerId;
    var chestItemId = args.chestItemId;

    // Consume chest item
    try {
        var inv = server.GetUserInventory({ PlayFabId: playerId });
        var chestItem = null;
        for (var i = 0; i < inv.Inventory.length; i++) {
            if (inv.Inventory[i].ItemId === chestItemId) {
                chestItem = inv.Inventory[i];
                break;
            }
        }
        if (!chestItem) {
            return { success: false, error: "Chest item not in inventory: " + chestItemId };
        }

        server.RevokeInventoryItem({
            PlayFabId: playerId,
            ItemInstanceId: chestItem.ItemInstanceId
        });
    } catch (e) {
        return { success: false, error: "Failed to consume chest: " + e.message };
    }

    // Void Chest pity-guaranteed VC on 60th open (tracked via stats)
    var grantedItems = [];
    var roll = Math.random();

    // Basic loot table (expand in future)
    if (roll < 0.05) {
        server.AddUserVirtualCurrency({ PlayFabId: playerId, VirtualCurrency: "VC", Amount: 50 });
        grantedItems.push("50 Void Crystals");
    } else if (roll < 0.30) {
        server.AddUserVirtualCurrency({ PlayFabId: playerId, VirtualCurrency: "PG", Amount: 2000 });
        grantedItems.push("2000 Pale Gold");
    } else {
        server.AddUserVirtualCurrency({ PlayFabId: playerId, VirtualCurrency: "PG", Amount: 500 });
        grantedItems.push("500 Pale Gold");
    }

    log.info("OpenChest: player=" + playerId + " granted=" + JSON.stringify(grantedItems));
    return { success: true, grantedItems: grantedItems };
};
