using UnityEngine;
using UnityEngine.UI;
using Varjo;

public class VarjoCubeMapStreamView : MonoBehaviour
{

    public RawImage leftView;
    public RawImage rightView;

    public void UpdateViews(VarjoEnvironmentCubemapStream.VarjoEnvironmentCubemapFrame frame)
    {
        leftView.texture = frame.cubemap;
        rightView.texture = frame.cubemap;
    }
}