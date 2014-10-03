namespace Tesseract {

    public class EngineConfig {
        private string _dataPath;
        private string _language;

        public string DataPath {
            get {
                return this._dataPath;
            }
            set {
                this._dataPath = value;
            }
        }

        public string Language {
            get {
                return this._language;
            }
            set {
                this._language = value;
            }
        }
    }
}