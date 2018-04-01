
namespace ConfigTable.Editor {
    public interface IPoolableObject
    {

        void FromPool ();

        void ToPool ();
    }
}

