﻿using ColossalFramework.UI;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace NodeMarkup.Utils
{
    public static class TextureUtil
    {
        public static UITextureAtlas _inGameAtlas;
        public static UITextureAtlas _inMapEditorAtlas;
        public static UITextureAtlas InGameAtlas
        {
            get
            {
                if (_inGameAtlas == null)
                    _inGameAtlas = GetAtlas("Ingame");
                return _inGameAtlas;
            }
        }
        public static UITextureAtlas InMapEditorAtlas
                {
            get
            {
                if (_inMapEditorAtlas == null)
                    _inMapEditorAtlas = GetAtlas("InMapEditor");
                return _inMapEditorAtlas;
            }
}

static readonly string path = $"{nameof(NodeMarkup)}.Resources.";
        public static UITextureAtlas CreateTextureAtlas(string textureFile, string atlasName, int spriteWidth, int spriteHeight, string[] spriteNames)
        {
            Texture2D texture2D = LoadTextureFromAssembly(
                textureFile, spriteWidth * spriteNames.Length, spriteHeight);

            UITextureAtlas uitextureAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            //Assert(uitextureAtlas != null, "uitextureAtlas");
            Material material = Object.Instantiate<Material>(UIView.GetAView().defaultAtlas.material);
            //Assert(material != null, "material");
            material.mainTexture = texture2D;
            uitextureAtlas.material = material;
            uitextureAtlas.name = atlasName;
            int num2;
            for (int i = 0; i < spriteNames.Length; i = num2)
            {
                float num = 1f / (float)spriteNames.Length;
                UITextureAtlas.SpriteInfo spriteInfo = new UITextureAtlas.SpriteInfo
                {
                    name = spriteNames[i],
                    texture = texture2D,
                    region = new Rect((float)i * num, 0f, num, 1f)
                };
                uitextureAtlas.AddSprite(spriteInfo);
                num2 = i + 1;
            }
            return uitextureAtlas;
        }

        public static void AddTexturesInAtlas(UITextureAtlas atlas, Texture2D[] newTextures, bool locked = false)
        {
            Texture2D[] textures = new Texture2D[atlas.count + newTextures.Length];

            for (int i = 0; i < atlas.count; i++)
            {
                Texture2D texture2D = atlas.sprites[i].texture;

                if (locked)
                {
                    // Locked textures workaround
                    RenderTexture renderTexture = RenderTexture.GetTemporary(texture2D.width, texture2D.height, 0);
                    Graphics.Blit(texture2D, renderTexture);

                    RenderTexture active = RenderTexture.active;
                    texture2D = new Texture2D(renderTexture.width, renderTexture.height);
                    RenderTexture.active = renderTexture;
                    texture2D.ReadPixels(new Rect(0f, 0f, (float)renderTexture.width, (float)renderTexture.height), 0, 0);
                    texture2D.Apply();
                    RenderTexture.active = active;

                    RenderTexture.ReleaseTemporary(renderTexture);
                }

                textures[i] = texture2D;
                textures[i].name = atlas.sprites[i].name;
            }

            for (int i = 0; i < newTextures.Length; i++)
                textures[atlas.count + i] = newTextures[i];

            Rect[] regions = atlas.texture.PackTextures(textures, atlas.padding, 4096, false);

            atlas.sprites.Clear();

            for (int i = 0; i < textures.Length; i++)
            {
                UITextureAtlas.SpriteInfo spriteInfo = atlas[textures[i].name];
                atlas.sprites.Add(new UITextureAtlas.SpriteInfo
                {
                    texture = textures[i],
                    name = textures[i].name,
                    border = (spriteInfo != null) ? spriteInfo.border : new RectOffset(),
                    region = regions[i]
                });
            }

            atlas.RebuildIndexes();
        }

        public static UITextureAtlas GetAtlas(string name)
        {
            UITextureAtlas[] atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
            for (int i = 0; i < atlases.Length; i++)
            {
                if (atlases[i].name == name)
                    return atlases[i];
            }
            return UIView.GetAView().defaultAtlas;
        }

        public static Texture2D LoadTextureFromAssembly(string textureFile, int width, int height)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string path = TextureUtil.path + textureFile;
            Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(path);
            //Assert(manifestResourceStream != null, "could not find " + path);
            byte[] array = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(array, 0, array.Length);

            Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
            //Assert(texture2D != null, "texture2D");
            texture2D.filterMode = FilterMode.Bilinear;
            texture2D.LoadImage(array);
            texture2D.Apply(true, true);

            return texture2D;
        }

        public static Texture2D GetTextureFromAssemblyManifest(string file)
        {
            string path = string.Concat(TextureUtil.path, file);
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
            {
                byte[] array = new byte[manifestResourceStream.Length];
                manifestResourceStream.Read(array, 0, array.Length);
                texture2D.LoadImage(array);
            }
            texture2D.wrapMode = TextureWrapMode.Clamp;
            texture2D.Apply();
            return texture2D;
        }

    }
}
