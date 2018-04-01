using System;


namespace FunEngine.Core
{
    public class AbstractConfVO : IDisposable
    {

        public AbstractConfVO(int id = 0, string name = "", string description = "")
        {
            this.id = id;
            this.name = name;
            this.description = description;
        }

        ~AbstractConfVO()
        {
            this.Dispose();
        }

        public virtual void Dispose()
        {
        }

        // ------------------------------------------------------------------------------
        // 属性
        // ------------------------------------------------------------------------------
        public int id
        {
            get;
            set;
        }

        //public string resID
        //{
        //    get;
        //    set;
        //}

        public string name
        {
            get;
            set;
        }

        public string description
        {
            get;
            set;
        }

        public override string ToString ()
        {
            return this.GetType ().Name + ": id(" + this.id + "), name(" + this.name + "), description(" + this.description + ")";
        }

        // ------------------------------------------------------------------------------
        // 从数据池中填充数据
        // ------------------------------------------------------------------------------
        public void Fill ()
        {
            //VoManager.FillVo (this);
        }

        public virtual void OnData (object data)
        {
        }
    }
}
