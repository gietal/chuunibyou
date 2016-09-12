using UnityEngine;
using System.Collections;

namespace chuunibyou
{
    public class UnityChan_ComboManager : ComboManager
    {
        protected override void RegisterCombos()
        {
            RegisterCombo(new ComboAction_UnityChan_Light1(), 
                ComboActionType.LightAttack
            );
            //RegisterCombo(new ComboAction_Empty(), ComboActionType.LightAttack); // assert already exist
            //RegisterCombo(new ComboAction_Empty(), ComboActionType.LightAttack, ComboActionType.LightAttack, ComboActionType.LightAttack); // assert Light, Light combo doesnt exist

            RegisterCombo(new ComboAction_UnityChan_Light1(), 
                ComboActionType.LightAttack, 
                ComboActionType.LightAttack
            );

            RegisterCombo(new ComboAction_UnityChan_Light1(), 
                ComboActionType.LightAttack, 
                ComboActionType.LightAttack,
                ComboActionType.LightAttack
            );

            RegisterCombo(new ComboAction_UnityChan_Light1(), 
                ComboActionType.LightAttack, 
                ComboActionType.LightAttack,
                ComboActionType.LightAttack,
                ComboActionType.LightAttack
            );
        }
        
    }
}
