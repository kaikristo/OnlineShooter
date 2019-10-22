using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    Image hpBar;
    [SerializeField]
    Text killCount;

    public void RefreshHP(float value)
    {

        if (value == 0)
        {
            hpBar.fillAmount = 0;
            return;
        }
        hpBar.fillAmount = (value / 100);
    }

    internal void RefreshKillCount(int killCount)
    {
        this.killCount.text = "Убито : " + killCount;
    }
}
