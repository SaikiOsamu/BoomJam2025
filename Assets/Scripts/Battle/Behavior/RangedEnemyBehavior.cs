using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleEntity;

class RangedEnemyBehavior : BaseBehavior
{
    public override MoveDelegate MoveDelegate => new ChasePlayerMoveHandler(definitions.moveSpeed, definitions.attackDistance).Move;
    public override AttackDelegate AttackDelegate => new NearPlayerSkillHandler(definitions.attackDistance).Attack;
    public override SelfDestructDelegate SelfDestructDelegate => new LifeBasedSelfDestructHandler().Update;

    BehaviorDefinitions definitions;

    public RangedEnemyBehavior(BehaviorDefinitions definitions)
    {
        this.definitions = definitions;
    }
}