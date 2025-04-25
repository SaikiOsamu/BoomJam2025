using UnityEngine;
public class SacrificeSystem : MonoBehaviour
{
    public InteractionPanelController interactionPanel;  // 弹窗控制器


    void Update()
    {
        // 检查玩家是否按下了 P 键
        if (Input.GetKeyDown(KeyCode.P))
        {   
            Debug.Log("P button");
            TryOpenSacrificePanel();  // 按下 P 键时触发献祭确认面板
        }
    }
    
    // 打开献祭确认弹窗
    public void TryOpenSacrificePanel()
    {
        interactionPanel.Show(
            $"是否消耗 X 能量换取 Y 净化值？",  // 消息
            OnConfirmSacrifice  // 确认按钮的回调
        );
    }

    // 确认献祭回调
    private void OnConfirmSacrifice()
    {
        //TODO deal Sacrifice
        bool success = true;
        string resultMessage = success ? "献祭成功！" : "能量不足，献祭失败！";
        Debug.Log(resultMessage);
        interactionPanel.ShowFloatingText(resultMessage);
    }

    // 取消献祭回调
    private void OnCancelSacrifice()
    {
        Debug.Log("取消献祭！");
    }
}
