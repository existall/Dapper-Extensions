using System;
using System.Collections.Generic;
using System.Linq;
using ExistsForAll.DapperExtensions.Mapper;

namespace DapperExtensions.Test.Data
{
    public class Multikey
    {
        public int Key1 { get; set; } 
        public string Key2 { get; set; }
        public string Value { get; set; }
        //public DateTime Date { get; set; }
    }

    public class MultikeyMapper : ClassMapper<Multikey>
    {
        public MultikeyMapper()
        {
	        Key(p => p.Key1);
	        Key(p => p.Key2).GeneratedBy.Assigned();
            //Map(p => p.Date).Ignore();
			Map(x=>x.Value);
        }
    }
}