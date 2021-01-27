using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DotsRogue.Authoring
{
    [CustomPreview(typeof(MapTileAuthoring))]
    public class MapTileAuthoringPreview : ObjectPreview
    {
        Sprite[] _sprites;

        Sprite[] GetSprites(MapTileAuthoring tar)
        {
            if (_sprites == null)
            {
                var mat = Resources.Load<Material>("Terminal8x8");
                if (mat == null || mat.mainTexture == null)
                    return null;

                var texPath = AssetDatabase.GetAssetPath(mat.mainTexture);
                var assets = AssetDatabase.LoadAllAssetsAtPath(texPath);
                if (assets == null || assets.Length == 0)
                    return null;

                var sprites = assets.Where(q => q is Sprite).Cast<Sprite>().ToArray();
                int maxLen = sprites.Max(x => x.name.Length);
                sprites = sprites.OrderBy(s => s.name.PadLeft(maxLen, '0')).ToArray();
                _sprites = sprites;
            }
            return _sprites;
        }

        public override bool HasPreviewGUI()
        {
            var tar = target as MapTileAuthoring;
            _sprites = GetSprites(tar);
            return tar != null && _sprites != null &&
                tar.GlyphIndex >= 0 && tar.GlyphIndex < _sprites.Length;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            var tar = target as MapTileAuthoring;
            if (tar == null)
                return;

            // Assumes a "Code Page 437 Sprite Sheet". 16 sprites x 16 sprites,
            // all the same size.
            var sprites = GetSprites(tar);
            int i = tar.GlyphIndex;
            if (sprites == null || i < 0 || i >= sprites.Length)
                return;

            var sprite = sprites[i];
            var tex = AssetPreview.GetAssetPreview(sprite);

            Draw(r, Texture2D.whiteTexture, tar.BackgroundColor, ScaleMode.StretchToFill);
            Draw(r, tex, tar.ForegroundColor, ScaleMode.ScaleToFit);
        }

        void Draw(Rect r, Texture tex, Color col, ScaleMode scaleMode)
        {
            if (tex == null)
                return;

            tex.filterMode = FilterMode.Point;

            var guiFG = GUI.color;

            GUI.color = col;
            GUI.DrawTexture(r, tex, scaleMode);

            GUI.color = guiFG;
        }
    }  
}