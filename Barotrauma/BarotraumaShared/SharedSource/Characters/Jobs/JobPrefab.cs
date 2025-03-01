﻿using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Barotrauma
{
    public class AutonomousObjective
    {
        public string identifier;
        public string option;
        public readonly float priorityModifier;
        public readonly bool ignoreAtOutpost;

        public AutonomousObjective(XElement element)
        {
            identifier = element.GetAttributeString("identifier", null);

            //backwards compatibility
            if (string.IsNullOrEmpty(identifier))
            {
                identifier = element.GetAttributeString("aitag", null);
            }

            option = element.GetAttributeString("option", null);
            priorityModifier = element.GetAttributeFloat("prioritymodifier", 1);
            priorityModifier = MathHelper.Max(priorityModifier, 0);
            ignoreAtOutpost = element.GetAttributeBool("ignoreatoutpost", false);
        }
    }

    partial class JobPrefab : IPrefab, IDisposable
    {
        public static readonly PrefabCollection<JobPrefab> Prefabs = new PrefabCollection<JobPrefab>();

        private bool disposed = false;
        public void Dispose()
        {
            if (disposed) { return; }
            disposed = true;
            Prefabs.Remove(this);
        }

        private static readonly Dictionary<string, float> _itemRepairPriorities = new Dictionary<string, float>();
        /// <summary>
        /// Tag -> priority.
        /// </summary>
        public static IReadOnlyDictionary<string, float> ItemRepairPriorities => _itemRepairPriorities;

        public static XElement NoJobElement;
        public static JobPrefab Get(string identifier)
        {
            if (Prefabs == null)
            {
                DebugConsole.ThrowError("Issue in the code execution order: job prefabs not loaded.");
                return null;
            }
            if (Prefabs.ContainsKey(identifier))
            {
                return Prefabs[identifier];
            }
            else
            {
                DebugConsole.ThrowError("Couldn't find a job prefab with the given identifier: " + identifier);
                return null;
            }
        }

        public class PreviewItem
        {
            public readonly string ItemIdentifier;
            public readonly bool ShowPreview;

            public PreviewItem(string itemIdentifier, bool showPreview)
            {
                ItemIdentifier = itemIdentifier;
                ShowPreview = showPreview;
            }
        }

        public readonly Dictionary<int, XElement> ItemSets = new Dictionary<int, XElement>();
        public readonly Dictionary<int, List<PreviewItem>> PreviewItems = new Dictionary<int, List<PreviewItem>>();
        public readonly List<SkillPrefab> Skills = new List<SkillPrefab>();
        public readonly List<AutonomousObjective> AutonomousObjectives = new List<AutonomousObjective>();
        public readonly List<string> AppropriateOrders = new List<string>();

        [Serialize("1,1,1,1", false)]
        public Color UIColor
        {
            get;
            private set;
        }

        [Serialize("notfound", false)]
        public string Identifier
        {
            get;
            private set;
        }

        [Serialize("notfound", false)]
        public string Name
        {
            get;
            private set;
        }

        [Serialize(AIObjectiveIdle.BehaviorType.Passive, false)]
        public AIObjectiveIdle.BehaviorType IdleBehavior
        {
            get;
            private set;
        }

        public string OriginalName { get { return Identifier; } }

        public ContentPackage ContentPackage { get; private set; }

        [Serialize("", false)]
        public string Description
        {
            get;
            private set;
        }

        [Serialize(false, false)]
        public bool OnlyJobSpecificDialog
        {
            get;
            private set;
        }

        //the number of these characters in the crew the player starts with in the single player campaign
        [Serialize(0, false)]
        public int InitialCount
        {
            get;
            private set;
        }

        //if set to true, a client that has chosen this as their preferred job will get it no matter what
        [Serialize(false, false)]
        public bool AllowAlways
        {
            get;
            private set;
        }

        //how many crew members can have the job (only one captain etc) 
        [Serialize(100, false)]
        public int MaxNumber
        {
            get;
            private set;
        }

        //how many crew members are REQUIRED to have the job 
        //(i.e. if one captain is required, one captain is chosen even if all the players have set captain to lowest preference)
        [Serialize(0, false)]
        public int MinNumber
        {
            get;
            private set;
        }

        [Serialize(0.0f, false)]
        public float MinKarma
        {
            get;
            private set;
        }

        [Serialize(1.0f, false)]
        public float PriceMultiplier
        {
            get;
            private set;
        }

        // TODO: not used
        [Serialize(10.0f, false)]
        public float Commonness
        {
            get;
            private set;
        }

        //how much the vitality of the character is increased/reduced from the default value
        [Serialize(0.0f, false)]
        public float VitalityModifier
        {
            get;
            private set;
        }

        //whether the job should be available to NPCs
        [Serialize(false, false)]
        public bool HiddenJob
        {
            get;
            private set;
        }

        public Sprite Icon;
        public Sprite IconSmall;

        public SkillPrefab PrimarySkill => Skills?.FirstOrDefault(s => s.IsPrimarySkill);

        public string FilePath { get; private set; }

        public XElement Element { get; private set; }
        public XElement ClothingElement { get; private set; }
        public int Variants { get; private set; }

        public JobPrefab(XElement element, string filePath)
        {
            FilePath = filePath;
            SerializableProperty.DeserializeProperties(this, element);

            Name = TextManager.Get("JobName." + Identifier);
            Description = TextManager.Get("JobDescription." + Identifier, returnNull: true) ?? string.Empty;
            Identifier = Identifier.ToLowerInvariant();
            Element = element;

            int variant = 0;
            foreach (XElement subElement in element.Elements())
            {
                switch (subElement.Name.ToString().ToLowerInvariant())
                {
                    case "itemset":
                        ItemSets.Add(variant, subElement);
                        PreviewItems[variant] = new List<PreviewItem>();
                        loadItemIdentifiers(subElement, variant);
                        variant++;
                        break;
                    case "skills":
                        foreach (XElement skillElement in subElement.Elements())
                        {
                            Skills.Add(new SkillPrefab(skillElement));
                        }
                        break;
                    case "autonomousobjectives":
                        subElement.Elements().ForEach(order => AutonomousObjectives.Add(new AutonomousObjective(order)));
                        break;
                    case "appropriateobjectives":
                    case "appropriateorders":
                        subElement.Elements().ForEach(order => AppropriateOrders.Add(order.GetAttributeString("identifier", "").ToLowerInvariant()));
                        break;
                    case "jobicon":
                        Icon = new Sprite(subElement.FirstElement());
                        break;
                    case "jobiconsmall":
                        IconSmall = new Sprite(subElement.FirstElement());
                        break;
                }
            }

            void loadItemIdentifiers(XElement parentElement, int variant)
            {
                foreach (XElement itemElement in parentElement.GetChildElements("Item"))
                {
                    if (itemElement.Element("name") != null)
                    {
                        DebugConsole.ThrowError("Error in job config \"" + Name + "\" - use identifiers instead of names to configure the items.");
                        continue;
                    }

                    string itemIdentifier = itemElement.GetAttributeString("identifier", "");
                    if (string.IsNullOrWhiteSpace(itemIdentifier))
                    {
                        DebugConsole.ThrowError("Error in job config \"" + Name + "\" - item with no identifier.");
                    }
                    else
                    {
                        PreviewItems[variant].Add(new PreviewItem(itemIdentifier, itemElement.GetAttributeBool("showpreview", true)));
                    }
                    loadItemIdentifiers(itemElement, variant);
                }
            }

            Variants = variant;

            Skills.Sort((x,y) => y.LevelRange.X.CompareTo(x.LevelRange.X));

            // Disabled on purpose, TODO: remove all references?
            //ClothingElement = element.GetChildElement("PortraitClothing");
        }
        

        public static JobPrefab Random(Rand.RandSync sync = Rand.RandSync.Unsynced) => Prefabs.GetRandom(p => !p.HiddenJob, sync);

        public static void LoadAll(IEnumerable<ContentFile> files)
        {
            foreach (ContentFile file in files)
            {
                LoadFromFile(file);
            }
        }

        public static void LoadFromFile(ContentFile file)
        {
            XDocument doc = XMLExtensions.TryLoadXml(file.Path);
            if (doc == null) { return; }
            var mainElement = doc.Root.IsOverride() ? doc.Root.FirstElement() : doc.Root;
            if (doc.Root.IsOverride())
            {
                DebugConsole.ThrowError($"Error in '{file.Path}': Cannot override all job prefabs, because many of them are required by the main game! Please try overriding jobs one by one.");
            }
            foreach (XElement element in mainElement.Elements())
            {
                if (element.IsOverride())
                {
                    var job = new JobPrefab(element.FirstElement(), file.Path)
                    {
                        ContentPackage = file.ContentPackage
                    };
                    Prefabs.Add(job, true);
                }
                else
                {
                    if (!element.Name.ToString().Equals("job", StringComparison.OrdinalIgnoreCase)) { continue; }
                    var job = new JobPrefab(element, file.Path)
                    {
                        ContentPackage = file.ContentPackage
                    };
                    Prefabs.Add(job, false);
                }
            }
            NoJobElement ??= mainElement.GetChildElement("nojob");
            var itemRepairPrioritiesElement = mainElement.GetChildElement("ItemRepairPriorities");
            if (itemRepairPrioritiesElement != null)
            {
                foreach (var subElement in itemRepairPrioritiesElement.Elements())
                {
                    string tag = subElement.GetAttributeString("tag", null);
                    if (tag != null)
                    {
                        float priority = subElement.GetAttributeFloat("priority", -1f);
                        if (priority >= 0)
                        {
                            _itemRepairPriorities.TryAdd(tag, priority);
                        }
                        else
                        {
                            DebugConsole.AddWarning($"The 'priority' attribute is missing from the the item repair priorities definition in {subElement} of {file.Path}.");
                        }
                    }
                    else
                    {
                        DebugConsole.AddWarning($"The 'tag' attribute is missing from the the item repair priorities definition in {subElement} of {file.Path}.");
                    }
                }
            }
        }

        public static void RemoveByFile(string filePath)
        {
            Prefabs.RemoveByFile(filePath);
        }
    }
}
