﻿using ColossalFramework.UI;
using NodeMarkup.Manager;
using NodeMarkup.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NodeMarkup.UI.Editors
{
    public class PointsEditor : Editor<PointItem, MarkupPoint, ColorIcon>
    {
        public override string Name => NodeMarkup.Localize.PointEditor_Points;

        private FloatPropertyPanel Offset { get; set; }

        public PointsEditor()
        {
            SettingsPanel.autoLayoutPadding = new RectOffset(10, 10, 0, 0);
        }
        protected override void FillItems()
        {
#if STOPWATCH
            var sw = Stopwatch.StartNew();
#endif
            foreach (var enter in Markup.Enters)
            {
                foreach (var point in enter.Points)
                {
                    var item = AddItem(point);
                }
            }
#if STOPWATCH
            Logger.LogDebug($"{nameof(PointsEditor)}.{nameof(FillItems)}: {sw.ElapsedMilliseconds}ms");
#endif
        }
        protected override void OnObjectSelect()
        {
            Offset = SettingsPanel.AddUIComponent<FloatPropertyPanel>();
            Offset.Text = NodeMarkup.Localize.PointEditor_Offset;
            Offset.UseWheel = true;
            Offset.WheelStep = 0.1f;
            Offset.Init();
            Offset.Value = EditObject.Offset;
            Offset.OnValueChanged += OffsetChanged;
        }
        protected override void OnObjectUpdate()
        {
            Offset.OnValueChanged -= OffsetChanged;
            Offset.Value = EditObject.Offset;
            Offset.OnValueChanged += OffsetChanged;
        }
        private void OffsetChanged(float value) => EditObject.Offset = value;

        public override void Render(RenderManager.CameraInfo cameraInfo)
        {
            if (HoverItem != null)
            {
                NodeMarkupTool.RenderManager.OverlayEffect.DrawCircle(cameraInfo, Color.white, HoverItem.Object.Position, 2f, -1f, 1280f, false, true);
            }
        }
    }
    public class PointItem : EditableItem<MarkupPoint, ColorIcon>
    {
        public override string Description => NodeMarkup.Localize.PointEditor_ItemDescription;

        public PointItem() : base(true, false) { }

        protected override void OnObjectSet()
        {
            Icon.InnerColor = Object.Color;
        }
    }
}
