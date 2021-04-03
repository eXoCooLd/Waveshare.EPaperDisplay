namespace Waveshare.Interfaces
{
    /// <summary>
    /// Base generic Interface for ImageLoaders
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEPaperDisplayImage<T> : IEPaperDisplay
    {
        /// <summary>
        /// Display Image of a generic Type
        /// </summary>
        /// <param name="image"></param>
        void DisplayImage(T image);
    }
}