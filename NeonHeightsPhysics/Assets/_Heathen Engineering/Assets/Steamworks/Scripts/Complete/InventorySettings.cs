#if !DISABLESTEAMWORKS && HE_STEAMCOMPLETE
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    [Serializable]
    public class InventorySettings
    {
        public List<ItemDefinition> items = new List<ItemDefinition>();

        public void Load()
        {
            API.Inventory.Client.EventSteamInventoryDefinitionUpdate.AddListener(HandleDefinitionLoad);
            API.Inventory.Client.EventSteamInventoryResultReady.AddListener(HandleItemResults);
            API.Inventory.Client.GetAllItems();
        }

        public void UpdateItemDefinitions()
        {
            HandleDefinitionLoad();
        }

        private void HandleDefinitionLoad()
        {
            if (API.Inventory.Client.GetItemDefinitionIDs(out SteamItemDef_t[] results))
            {
                List<ItemDefinition> bundles = new List<ItemDefinition>();
                Dictionary<ItemDefinition, string> craftable = new Dictionary<ItemDefinition, string>();
                Dictionary<ItemDefinition, string> generators = new Dictionary<ItemDefinition, string>();

                for (int i = 0; i < results.Length; i++)
                {
                    try
                    {
                        var itemDefId = results[i];
                        var target = items.FirstOrDefault(p => p.item_itemdefid == itemDefId.m_SteamItemDef);
                        var created = false;
                        if (target == null)
                        {
                            created = true;
                            target = ScriptableObject.CreateInstance<ItemDefinition>();
                        }

                        target.item_itemdefid = itemDefId.m_SteamItemDef;

                        target.item_name.Populate(itemDefId);

                        var namePart = "";

                        if (!string.IsNullOrEmpty(target.item_name.value))
                            namePart = itemDefId.ToString() + " " + target.item_name.value;
                        else if (target.item_name.variants.Count > 0)
                            namePart = itemDefId.ToString() + " " + target.item_name.variants[0].value;
                        else
                            namePart = itemDefId.ToString() + " UNKNOWN";

                        target.name = "[Inv] " + namePart;

                        target.item_description.Populate(itemDefId);
                        target.item_display_type.Populate(itemDefId);

                        var type = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "type");
                        switch (type)
                        {
                            case "item":
                                target.item_type = Enums.InventoryItemType.item;
                                break;
                            case "bundle":
                                target.item_type = Enums.InventoryItemType.bundle;
                                bundles.Add(target);
                                break;
                            case "generator":
                                target.item_type = Enums.InventoryItemType.generator;
                                bundles.Add(target);
                                break;
                            case "playtimegenerator":
                                target.item_type = Enums.InventoryItemType.playtimegenerator;
                                bundles.Add(target);
                                break;
                            case "tag_generator":
                                target.item_type = Enums.InventoryItemType.tag_generator;
                                break;
                            default:
                                UnityEngine.Debug.LogWarning("Unknown Item Type: " + target.name);
                                break;
                        }

                        target.item_promo.Populate(itemDefId);
                        target.item_drop_start_time = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "drop_start_time");

                        var excahnge = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "exchange");
                        if (!string.IsNullOrEmpty(excahnge) && !craftable.ContainsKey(target))
                            craftable.Add(target, excahnge);

                        target.item_price.Populate(itemDefId);

                        var bgColor = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "background_color");
                        if (!string.IsNullOrEmpty(bgColor))
                        {
                            if (UnityEngine.ColorUtility.TryParseHtmlString(bgColor, out Color color))
                                target.item_background_color.color = color;
                            else
                                target.item_background_color.use = false;
                        }
                        else
                            target.item_background_color.use = false;

                        var nColor = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "name_color");
                        if (!string.IsNullOrEmpty(nColor))
                        {
                            if (UnityEngine.ColorUtility.TryParseHtmlString(nColor, out Color color))
                                target.item_name_color.color = color;
                            else
                                target.item_name_color.use = false;
                        }
                        else
                            target.item_name_color.use = false;

                        target.item_icon_url = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "icon_url");
                        target.item_icon_url_large = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "icon_url_large");

                        var returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "marketable");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool marketable))
                            target.item_marketable = marketable;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tradable");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool tradable))
                            target.item_tradable = tradable;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tag_generators");
                        if (!string.IsNullOrEmpty(returnString))
                            generators.Add(target, returnString);

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "store_hidden");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool store_hidden))
                            target.item_store_hidden = store_hidden;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "use_drop_limit");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool use_drop_limit))
                            target.item_use_drop_limit = use_drop_limit;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "use_drop_window");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_use_drop_window))
                            target.item_tradable = item_use_drop_window;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "granted_manually");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_granted_manually))
                            target.item_tradable = item_granted_manually;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "use_bundle_price");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_use_bundle_price))
                            target.item_tradable = item_use_bundle_price;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "auto_stack");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_auto_stack))
                            target.item_tradable = item_auto_stack;

                        target.item_tag_generator_name = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tag_generator_name");
                        target.item_tag_generator_values.Populate(itemDefId);
                        target.item_tags.Populate(itemDefId);

                        target.item_store_tags = new List<string>(API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "store_tags").Split(';'));
                        target.item_store_images = new List<string>(API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "store_images").Split(';'));

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tradabitem_drop_limitle");
                        if (!string.IsNullOrEmpty(returnString) && uint.TryParse(returnString, out uint item_drop_limit))
                            target.item_drop_limit = item_drop_limit;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "item_drop_interval");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_drop_interval))
                            target.item_tradable = item_drop_interval;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "item_drop_window");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_drop_window))
                            target.item_tradable = item_drop_window;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "item_drop_max_per_window");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_drop_max_per_window))
                            target.item_tradable = item_drop_max_per_window;

                        //TODO: check for and parse extended properties

                        if (created)
                        {
                            items.Add(target);
#if UNITY_EDITOR
                            UnityEditor.AssetDatabase.AddObjectToAsset(target, SteamSettings.current);
#endif
                        }
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(target);
                        UnityEditor.EditorUtility.SetDirty(SteamSettings.current);
                        UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(SteamSettings.current));
                        UnityEditor.EditorUtility.SetDirty(target);
#endif

                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Failed to parse item definition load from Valve: " + ex.Message);
                    }
                }

                for (int i = 0; i < bundles.Count; i++)
                {
                    try
                    {
                        var target = bundles[i];
                        target.item_bundle.entries = new List<ItemDefinition.Bundle.Entry>();
                        var bundleData = API.Inventory.Client.GetItemDefinitionProperty(target.Id, "bundle");
                        if (!string.IsNullOrEmpty(bundleData))
                        {
                            var recipes = bundleData.Split(';');
                            for (int ii = 0; ii < recipes.Length; ii++)
                            {
                                var recipe = recipes[ii];
                                if (recipe.Contains("x"))
                                {
                                    var kvp = recipe.Split('x');
                                    var id = int.Parse(kvp[0]);
                                    var count = int.Parse(kvp[1]);
                                    target.item_bundle.entries.Add(new ItemDefinition.Bundle.Entry
                                    {
                                        count = count,
                                        item = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id)
                                    });
                                }
                                else
                                {
                                    var id = int.Parse(recipe);
                                    target.item_bundle.entries.Add(new ItemDefinition.Bundle.Entry
                                    {
                                        count = 0,
                                        item = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id)
                                    });
                                }
                            }
                        }

#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(target);
                        UnityEditor.EditorUtility.SetDirty(SteamSettings.current);
                        UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(SteamSettings.current));
                        UnityEditor.EditorUtility.SetDirty(target);
#endif
                    }
                    catch(Exception ex)
                    {
#pragma warning disable UNT0008 // Null propagation on Unity objects
                        Debug.LogError("Failed to process bundle information for " + bundles[i]?.ToString() + "\nException: " + ex.Message);
#pragma warning restore UNT0008 // Null propagation on Unity objects
                    }
                }

                foreach (var keyValuePair in craftable)
                {
                    
                    var target = keyValuePair.Key;
                    var schema = keyValuePair.Value;
                    try
                    {
                        target.item_exchange = new ItemDefinition.ExchangeCollection();
                        target.item_exchange.recipe = new List<ItemDefinition.ExchangeRecipe>();
                        var recipies = schema.Split(';');
                        foreach (var recipe in recipies)
                        {
                            var recipieObject = new ItemDefinition.ExchangeRecipe();
                            var materials = recipe.Split(',');
                            foreach (var material in materials)
                            {
                                if (material.Contains(":"))
                                {
                                    //Tag
                                    var tagCat = material.Split(':');
                                    var catigory = tagCat[0];
                                    var tag = string.Empty;
                                    uint count = 1;
                                    if (tagCat[1].Contains("*"))
                                    {
                                        var tagCount = tagCat[1].Split('*');
                                        tag = tagCount[0];
                                        count = uint.Parse(tagCount[1]);
                                    }
                                    else
                                        tag = tagCat[1];

                                    recipieObject.materials.Add(new ItemDefinition.ExchangeRecipe.Material
                                    {
                                        item = new ItemDefinition.ExchangeRecipe.Material.Item_Def_Descriptor { item = null, count = 0 },
                                        tag = new ItemDefinition.ExchangeRecipe.Material.Item_Tag_Descriptor
                                        {
                                            name = catigory,
                                            value = tag,
                                            count = count
                                        }
                                    });
                                }
                                else
                                {
                                    //Item
                                    if (material.Contains("x"))
                                    {
                                        //Has count
                                        var itemCount = material.Split('x');
                                        var itemID = int.Parse(itemCount[0]);
                                        var count = uint.Parse(itemCount[1]);
                                        var itemTarget = items.FirstOrDefault(p => p.Id.m_SteamItemDef == itemID);

                                        recipieObject.materials.Add(new ItemDefinition.ExchangeRecipe.Material
                                        {
                                            item = new ItemDefinition.ExchangeRecipe.Material.Item_Def_Descriptor
                                            {
                                                item = itemTarget,
                                                count = count
                                            },
                                            tag = new ItemDefinition.ExchangeRecipe.Material.Item_Tag_Descriptor
                                            {
                                                name = string.Empty,
                                                value = string.Empty,
                                                count = 0
                                            }
                                        });
                                    }
                                    else
                                    {
                                        var itemID = int.Parse(material);
                                        var itemTarget = items.FirstOrDefault(p => p.Id.m_SteamItemDef == itemID);

                                        recipieObject.materials.Add(new ItemDefinition.ExchangeRecipe.Material
                                        {
                                            item = new ItemDefinition.ExchangeRecipe.Material.Item_Def_Descriptor
                                            {
                                                item = itemTarget,
                                                count = 1
                                            },
                                            tag = new ItemDefinition.ExchangeRecipe.Material.Item_Tag_Descriptor
                                            {
                                                name = string.Empty,
                                                value = string.Empty,
                                                count = 0
                                            }
                                        });
                                    }
                                }
                            }

                            target.item_exchange.recipe.Add(recipieObject);
                        }
                    }
                    catch (Exception ex)
                    {
#pragma warning disable UNT0008 // Null propagation on Unity objects
                        Debug.LogError("Failed to parse excahnge schema for " + target?.ToString() + "; schema = " + schema + "; \n Exception = " + ex.Message);
#pragma warning restore UNT0008 // Null propagation on Unity objects
                    }
                }

                foreach(var keyValuePair in generators)
                {
                    var target = keyValuePair.Key;
                    var schema = keyValuePair.Value;

                    if(schema.Contains(";"))
                    {
                        target.item_tag_generators = new List<ItemDefinition>();
                        var gens = schema.Split(';');
                        foreach(var idString in gens)
                        {
                            var id = int.Parse(idString);
                            var targetTagGen = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id);
                            if (targetTagGen != null)
                            {
                                target.item_tag_generators.Add(targetTagGen);
                            }
                        }
                    }
                    else
                    {
                        // Only 1
                        var id = int.Parse(schema);
                        target.item_tag_generators = new List<ItemDefinition>();
                        var targetTagGen = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id);
                        if(targetTagGen != null)
                        {
                            target.item_tag_generators.Add(targetTagGen);
                        }
                    }
                }

#if !UNITY_EDITOR
                API.Inventory.Client.GetAllItems(HandleItemResults);
#endif
            }
        }

        private void HandleItemResults(InventoryResult results)
        {
            foreach(var detail in results.items)
            {
                var def = items.FirstOrDefault(p => p.item_itemdefid == detail.Definition.m_SteamItemDef);
                if(def != null)
                {
                    def.Details.RemoveAll(p => p.ItemId == detail.ItemId);
                    def.Details.Add(detail);
                }
            }
        }
    }
}
#endif