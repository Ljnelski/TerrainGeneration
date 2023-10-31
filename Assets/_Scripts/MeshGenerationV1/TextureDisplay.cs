using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureDisplay : MonoBehaviour
{
    [SerializeField] private Renderer _textureRender;
    public void DrawTexture(Texture2D texture)
    {
        _textureRender.sharedMaterial.mainTexture = texture;
        //_textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }


}
