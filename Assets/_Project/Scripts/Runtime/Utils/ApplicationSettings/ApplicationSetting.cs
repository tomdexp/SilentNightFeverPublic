using QFSW.QC;

namespace _Project.Scripts.Runtime.Utils.ApplicationSettings
{
    public abstract class ApplicationSetting<T>
    {
        protected string key;
        protected T defaultValue;

        public T Value { get; protected set; }

        public ApplicationSetting(T defaultValue)
        {
            this.defaultValue = defaultValue;
            this.key = GetType().Name;  // Automatically sets the key based on the derived class name
            Load();
        }

        public abstract void Save();

        public virtual void Set(T value)
        {
            Logger.LogInfo($"Setting {key} from {Value} to {value}");
        }

        public virtual void CommandGet()
        {
            Logger.LogInfo($"Options {key} is " + Value);
        }
        public abstract void Load();

        public void ResetToDefault()
        {
            Value = defaultValue;
            Save();
        }
    }
}