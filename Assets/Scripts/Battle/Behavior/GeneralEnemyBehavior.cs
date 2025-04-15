using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static BattleEntity;

class GeneralEnemyBehavior : BaseBehavior
{
    public override MoveDelegate MoveDelegate => new ChasePlayerMoveHandler(definitions.moveSpeed).Move;
    public override AttackDelegate AttackDelegate => new NearPlayerAttackHandler().Attack;
    public override SelfDestructDelegate SelfDestructDelegate => new LifeBasedSelfDestructHandler().Update;

    BehaviorDefinitions definitions;

    public GeneralEnemyBehavior(BehaviorDefinitions definitions)
    {
        this.definitions = definitions;
    }
}