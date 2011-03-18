﻿/*                              
   Copyright 2010 Sven Rech (svenrech at googlemail dot com), Uni Duisburg-Essen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeySearcher.KeyPattern
{
    public class IntervalKeyMovement : IKeyMovement
    {
        public List<int> IntervalList
        {
            get;
            private set;
        }

        public IntervalKeyMovement(List<int> intervalList)
        {
            this.IntervalList = intervalList;
            this.IntervalList.Sort();
        }

        public int Count()
        {
            return IntervalList.Count;
        }
    }
}
