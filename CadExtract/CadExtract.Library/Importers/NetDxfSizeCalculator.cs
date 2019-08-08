using CadExtract.Library.Geometry;
using netDxf.Entities;
using System;
using System.Numerics;

namespace CadExtract.Library.Importers
{
    class NetDxfSizeCalculator
    {
        private const float DEFAULT_HEIGHT = 0.08f;
        private const float DEFAULT_WIDTH_RATIO = 0.9f;
        private const float WIDTH_PADDING = 0.08f;

        private static float CalculateFontHeight(float? fontHeight) => fontHeight ?? DEFAULT_HEIGHT;

        public static float CalculateTotalWidth(string text, float? fontHeight)
        {
            var h = CalculateFontHeight(fontHeight);
            return h * DEFAULT_WIDTH_RATIO * text.Length;
        }

        private static float CalculateWidth(string text, float? textWidthCache, float? textWidth, float? fontHeight)
        {
            var w = textWidthCache ?? textWidth ?? CalculateTotalWidth(text, fontHeight);
            return w + WIDTH_PADDING;
        }

        private static float CalculateHeight(bool isMText, string text, float? textHeightCache, float? textWidthCache, float? textWidth, float? fontHeight)
        {
            if (isMText)
            {
                var h = textHeightCache;
                if (h != null) { return h.Value; }

                var boxWidth = CalculateWidth(text, textWidthCache, textWidth, fontHeight);
                var totalWidth = CalculateTotalWidth(text, fontHeight);
                var lineCount = (float)Math.Ceiling(1.0f * totalWidth / boxWidth);
                return lineCount * CalculateFontHeight(fontHeight) * 1.6f;
            }
            else
            {
                return CalculateFontHeight(fontHeight);
            }
        }

        public static Vector2 CalculateCenter(float xLeft, float yBottom, float width, float height, bool isMText, MTextAttachmentPoint attachmentPoint)
        {
            if (isMText)
            {
                if (attachmentPoint == MTextAttachmentPoint.TopRight
                    || attachmentPoint == MTextAttachmentPoint.MiddleRight
                    || attachmentPoint == MTextAttachmentPoint.BottomRight)
                {
                    xLeft -= width;
                }

                if (attachmentPoint == MTextAttachmentPoint.TopCenter
                    || attachmentPoint == MTextAttachmentPoint.MiddleCenter
                    || attachmentPoint == MTextAttachmentPoint.BottomCenter)
                {
                    xLeft -= width * 0.5f;
                }

                if (attachmentPoint == MTextAttachmentPoint.MiddleRight
                    || attachmentPoint == MTextAttachmentPoint.MiddleCenter
                    || attachmentPoint == MTextAttachmentPoint.MiddleLeft)
                {
                    yBottom -= height * 0.5f;
                }

                if (attachmentPoint == MTextAttachmentPoint.TopRight
                    || attachmentPoint == MTextAttachmentPoint.TopCenter
                    || attachmentPoint == MTextAttachmentPoint.TopLeft)
                {
                    yBottom -= height;
                }
            }
            else
            {
                // Single line uses top left attachment point
                yBottom -= height;
            }

            //var h = HorizontalTextJustification;
            //var v = VerticalTextJustification;

            //var x1 = GetDoubleValueOrNull(DxfPartKind.PosX);
            //var y1 = GetDoubleValueOrNull(DxfPartKind.PosY);
            //var x2 = GetDoubleValueOrNull(DxfPartKind.AltPosX);
            //var y2 = GetDoubleValueOrNull(DxfPartKind.AltPosY);

            //if (!IsMText)
            //{
            //    // Even if center, the 1st point is still bottom left (so use it even though the alignment is not perfect)
            //    //if (h == DxfValue_Text72_HorizontalTextJustification.Center)
            //    //{
            //    //    return new Vector2(x1.Value, y1.Value);
            //    //}

            //    // Bottom Left
            //    return new Vector2(x1.Value + width * 0.5, y1.Value + height * 0.5);
            //}

            // Bottom Left
            return new Vector2(xLeft + width * 0.5f, yBottom + height * 0.5f);
        }

        public static Bounds CalculateBounds(float xLeft, float yBottom, bool isMText, string text, float? textHeightCache = null, float? textWidthCache = null, float? textWidth = null, float? fontHeight = null, MTextAttachmentPoint attachmentPoint = MTextAttachmentPoint.TopLeft)
        {
            var width = CalculateWidth(text, textWidthCache, textWidth == 0 ? null : textWidth, fontHeight);
            var height = CalculateHeight(isMText, text, textHeightCache, textWidthCache, width, fontHeight);
            var center = CalculateCenter(xLeft, yBottom, width, height, isMText, attachmentPoint);

            return Bounds.FromCenterSize(center, new Vector2(width, height));
        }
    }
}
