using System.Collections.Generic;
using System.Linq;
using ApiInspector.Application;

namespace ApiInspector.Components.KeyValueEditor
{
    public  class SuggestionHandler
    {
        public IReadOnlyList<CodeWithDescriptionContract> Handle(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return null;
            }

            var source = new BoaUserDataSource(new ConnectionString());

            return source.GetUsers(filter).Select(x => new CodeWithDescriptionContract {Code = x.UserCode, Description = x.Name}).ToList();
        }
    }
}