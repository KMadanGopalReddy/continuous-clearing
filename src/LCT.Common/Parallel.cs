﻿// --------------------------------------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: 2023 Siemens AG
//
//  SPDX-License-Identifier: MIT

// -------------------------------------------------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LCT.Common
{
    public static class DefinedParallel
    {
        public static Task ParallelForEachAsync<T>(this IEnumerable<T> totalreleases, Func<T, Task> body)
        {
            return Task.WhenAll(
             from item in totalreleases
             select Task.Run(() => body(item)));
        }
    }
}
