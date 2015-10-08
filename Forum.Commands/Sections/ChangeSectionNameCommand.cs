﻿using System;
using ENode.Commanding;

namespace Forum.Commands.Sections
{
    public class ChangeSectionNameCommand : Command
    {
        public string Name { get; set; }

        private ChangeSectionNameCommand() { }
        public ChangeSectionNameCommand(string id, string name) : base(id)
        {
            Name = name;
        }
    }
}
