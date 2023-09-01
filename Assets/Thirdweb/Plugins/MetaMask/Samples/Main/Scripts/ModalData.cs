namespace MetaMask.Unity.Samples
{
    public partial class ModalData
    {
        /// <summary>A type of modal dialog.</summary>
        public enum ModalType
        {
            Base,
            Alert,
            Error,
            Transaction
        }

        #region Fields

        /// <summary>The type of the modal.</summary>
        public ModalType type = ModalType.Base;
        /// <summary>The text of the header of the modal popup.</summary>
        public string headerText;
        /// <summary>The text of the body of the modal popup.</summary>
        public string bodyText;

        #endregion
    }
}