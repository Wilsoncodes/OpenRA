﻿#region Copyright & License Information
/*
 * Copyright 2007-2010 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made 
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see LICENSE.
 */
#endregion

using OpenRA.Mods.RA.Render;
using OpenRA.Traits;
using OpenRA.Traits.Activities;

namespace OpenRA.Mods.RA.Activities
{
	/* non-turreted attack */
	public class Attack : CancelableActivity
	{
		Target Target;
		int Range;

		public Attack(Target target, int range)
		{
			Target = target;
			Range = range;
		}

		public override IActivity Tick( Actor self )
		{
			if (IsCanceled) return NextActivity;
			var facing = self.Trait<IFacing>();
			if (!Target.IsValid)
				return NextActivity;

			var targetCell = Util.CellContaining(Target.CenterLocation);

			if ((targetCell - self.Location).LengthSquared >= Range * Range)
				return Util.SequenceActivities( new Move( Target, Range ), this );

			var desiredFacing = Util.GetFacing((targetCell - self.Location).ToFloat2(), 0);
			var renderUnit = self.TraitOrDefault<RenderUnit>();
			var numDirs = (renderUnit != null)
				? renderUnit.anim.CurrentSequence.Facings : 8;

			if (Util.QuantizeFacing(facing.Facing, numDirs) 
				!= Util.QuantizeFacing(desiredFacing, numDirs))
			{
				return Util.SequenceActivities( new Turn( desiredFacing ), this );
			}

			var attack = self.Trait<AttackBase>();
			attack.target = Target;
			attack.DoAttack(self);
			return this;
		}
	}
}
