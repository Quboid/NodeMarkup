﻿using ColossalFramework.UI;
using NodeMarkup.Manager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NodeMarkup.UI.Editors
{
    public class FillerEditor : Editor<FillerItem, MarkupFiller, StyleIcon>
    {
        public override string Name => NodeMarkup.Localize.FillerEditor_Fillers;

        public StylePropertyPanel Style { get; private set; }
        private StyleHeaderPanel Header { get; set; }
        private List<UIComponent> StyleProperties { get; set; } = new List<UIComponent>();

        public FillerEditor()
        {
            SettingsPanel.autoLayoutPadding = new RectOffset(10, 10, 0, 0);
        }
        protected override void FillItems()
        {
            foreach (var filler in Markup.Fillers)
            {
                AddItem(filler);
            }
        }
        protected override void OnObjectSelect()
        {
            AddHeader();
            AddStyleTypeProperty();
            AddStyleProperties();
        }

        private void AddHeader()
        {
            Header = SettingsPanel.AddUIComponent<StyleHeaderPanel>();
            Header.Init(Manager.Style.StyleType.Filler, false);
            Header.OnSaveTemplate += OnSaveTemplate;
            Header.OnSelectTemplate += OnSelectTemplate;
        }
        private void AddStyleTypeProperty()
        {
            Style = SettingsPanel.AddUIComponent<FillerStylePropertyPanel>();
            Style.Text = NodeMarkup.Localize.LineEditor_Style;
            Style.Init();
            Style.SelectedObject = EditObject.Style.Type;
            Style.OnSelectObjectChanged += StyleChanged;
        }
        private void AddStyleProperties()
        {
            StyleProperties = EditObject.Style.GetUIComponents(EditObject, SettingsPanel);
            if (StyleProperties.FirstOrDefault() is ColorPropertyPanel colorProperty)
                colorProperty.OnValueChanged += (Color32 c) => RefreshItem();
        }
        private void StyleChanged(Style.StyleType style)
        {
            if (style == EditObject.Style.Type)
                return;

            var newStyle = TemplateManager.GetDefault<FillerStyle>(style);
            EditObject.Style.CopyTo(newStyle);

            EditObject.Style = newStyle;

            RefreshItem();
            ClearStyleProperties();
            AddStyleProperties();
        }

        private void OnSaveTemplate()
        {
            if (TemplateManager.AddTemplate(EditObject.Style, out StyleTemplate template))
                NodeMarkupPanel.EditTemplate(template);
        }
        private void OnSelectTemplate(StyleTemplate template)
        {
            if (template.Style.Copy() is FillerStyle newStyle)
            {
                newStyle.MedianOffset = EditObject.Style.MedianOffset;
                if (newStyle is ISimpleFiller newSimple && EditObject.Style is ISimpleFiller oldSimple)
                {
                    newSimple.Angle = oldSimple.Angle;
                }

                EditObject.Style = newStyle;
                Style.SelectedObject = EditObject.Style.Type;

                RefreshItem();
                ClearStyleProperties();
                AddStyleProperties();
            }
        }
        private void ClearStyleProperties()
        {
            foreach (var property in StyleProperties)
            {
                SettingsPanel.RemoveUIComponent(property);
                Destroy(property);
            }
        }


        public override void Render(RenderManager.CameraInfo cameraInfo)
        {
            if (IsHoverItem)
            {
                foreach (var trajectory in HoverItem.Object.Trajectories)
                    NodeMarkupTool.RenderManager.OverlayEffect.DrawBezier(cameraInfo, Color.white, trajectory, 0.5f, 0f, 0f, -1f, 1280f, false, true);
            }
        }
        private void RefreshItem() => SelectItem.Refresh();
        protected override void OnObjectDelete(MarkupFiller filler)
        {
            Markup.RemoveFiller(filler);
        }
    }
    public class FillerItem : EditableItem<MarkupFiller, StyleIcon>
    {
        public override string Description => NodeMarkup.Localize.FillerEditor_ItemDescription;

        public FillerItem() : base(true, true) { }

        protected override void OnObjectSet() => SetIcon();
        public override void Refresh()
        {
            base.Refresh();
            SetIcon();
        }
        private void SetIcon()
        {
            Icon.Type = Object.Style.Type;
            Icon.StyleColor = Object.Style.Color;
        }
    }
}
