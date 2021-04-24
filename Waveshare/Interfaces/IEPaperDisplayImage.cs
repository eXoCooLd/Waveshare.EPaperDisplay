namespace Waveshare.Interfaces
{
    /// <summary>
    /// Base generic Interface for ImageLoaders
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEPaperDisplayImage<T> : IEPaperDisplay
    {

        /// <summary>
        /// Display a Image on the Display
        /// </summary>
        /// <param name="image">Image that should be displayed</param>
        /// <param name="dithering">Use Dithering</param>
        void DisplayImage(T image, bool dithering);

        /// <summary>
        /// Display Image of a generic Type
        /// </summary>
        /// <param name="image">Image that should be displayed</param>
        void DisplayImage(T image);

        /// <summary>
        /// Display Image of a generic Type with dithering
        /// </summary>
        /// <param name="image">Image that should be displayed</param>
        void DisplayImageWithDithering(T image);
    }
}