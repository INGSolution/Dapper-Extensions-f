using System.ComponentModel.DataAnnotations.Schema;

namespace DapperExtensions
{
    public abstract class BaseEntity
    {
        private string _guid;

        public virtual string Guid() 
        {
            if (string.IsNullOrEmpty(_guid))
            {
                _guid = System.Guid.NewGuid().ToString();
            }
            return _guid;
        }
    }
}
