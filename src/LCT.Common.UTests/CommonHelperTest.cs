﻿// --------------------------------------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: 2023 Siemens AG
//
//  SPDX-License-Identifier: MIT

// -------------------------------------------------------------------------------------------------------------------- 

using CycloneDX.Models;
using LCT.Common.Model;
using NUnit.Framework;
using System.Collections.Generic;

namespace LCT.Common.UTest
{
    public class CommonHelperTest
    {
        [Test]
        public void WriteComponentsNotLinkedListInConsole_PassingList_ReturnSuccess()
        {
            //Arrange
            List<Components> ComponentsNotLinked = new List<Components>();
            ComponentsNotLinked.Add(new Components());

            //Act
            CommonHelper.WriteComponentsNotLinkedListInConsole(ComponentsNotLinked);

            //Assert
            Assert.IsTrue(true);
        }

        [Test]
        public void RemoveExcludedComponents_PassingList_ReturnSuccess()
        {
            //Arrange
            List<Component> ComponentsForBom = new List<Component>();
            ComponentsForBom.Add(new Component() { Name = "Name", Version = "12" });
            int noOfExcludedComponents = 0;

            List<string> list = new List<string>();
            list.Add("Debian:Debian");

            //Act
            List<Component> result = CommonHelper.RemoveExcludedComponents(ComponentsForBom, list, ref noOfExcludedComponents);

            //Assert
            Assert.IsTrue(result.Count > 0);
        }
    }
}
