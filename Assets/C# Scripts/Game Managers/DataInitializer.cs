using UnityEngine;


public class DataInitializer : MonoBehaviour
{
    [SerializeField] private DefaultPlayerStatsSO defPlayerStats;

    [SerializeField] private StatusEffectSettingsSO statusEffectsRulesSO;
    [SerializeField] private DefenseSettingsSO defenseRulesSO;

    [SerializeField] private GlobalWeaponListSO globalWeaponListSO;
    [SerializeField] private GlobalSkillListSO globalSkillListSO;

    [SerializeField] private MatchSettingsSO matchSettingsSO;


    private void Awake()
    {
        GameRules.SetGameRules(defPlayerStats, statusEffectsRulesSO.StatusRules, defenseRulesSO.DefenseStrengthRules, matchSettingsSO.Value);

        SkillManager.Init(globalSkillListSO);
        WeaponManager.Init(globalWeaponListSO);
    }
}