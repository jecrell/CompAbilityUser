﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AbilityUser
{
    public class TargetAoEProperties
    {
        public bool showRangeOnSelect = true;
        public bool friendlyFire = false;
        public bool startsFromCaster = true;
        public int range;
        public int maxTargets = 3;
        public Type targetClass;
    }
}
