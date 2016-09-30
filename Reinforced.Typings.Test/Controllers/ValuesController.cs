using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Reinforced.Typings.Test.Models;
using Reinforced.Typings.Test.Models.New;
using Reinforced.Typings.Test.RT.Angular;

namespace Reinforced.Typings.Test.Controllers
{
    [Authorize]
    [AngularController]
    public class ValuesController : ApiController
    {
        // GET api/values
        [AngularMethod(typeof(Dummy))]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [AngularMethod(typeof(FancyModel[]))]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
