/* Help recording the story and export to an external file.
 */
using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;

namespace discordTRPGHelper
{
    public class StoryRecorder
    {
        /*
         * @brief The path of the directory where storing the story.
         */
        public const string outputDirectory = "./StoryRecord/";

        /*
         * @brief The filename of the output file.
         */
        private string _outputFilename = "";

        /*
         *  @brief Constructor.
         */
        public StoryRecorder()
        {
        }

        /*========= File operations ==========*/
        /*
         * @brief create a new file for saving the story.
         * @param filename Specify the saving filename. If not specified, using date time in default.
         *        Note that there is no need to add the file extension.
         * @return true If the file is created successfully.
         */
        public void CreateNewFile(string filename = "")
        {
            /* Create the directory first */
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            if (string.IsNullOrEmpty(filename))
                _outputFilename = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
            else
                _outputFilename = $"{filename}.txt";

            using (File.Create(Path.Combine(outputDirectory, _outputFilename))) { }
        }

        /*
         * @brief Get the filename of the current saving file.
         */
        public string GetCurrentSaveFilename()
        {
            return _outputFilename;
        }
    }

    /*
     * @brief The class for command system to access StoryRecorder.
     */
    public class StoryRecorderHelper : ModuleBase<SocketCommandContext>
    {
        /*
         * @brief The access to the StoryRecorder.
         */
        private StoryRecorder _storyRecorder;

        /*
         * @brief Constructor. Store the reference to the StoryRecorder;
         */
        public StoryRecorderHelper(StoryRecorder storyRecorder)
        {
            _storyRecorder = storyRecorder;
        }

        /*
         * @brief The slot for the command \"!createStory [story_name]\".
         *
         * Create a new file to record the story.
         */
        [Command("createStory")]
        [Alias("創造故事")]
        [Summary("Create a new file to record the story")]
        public async Task CreateStory([Summary("[Optional] Specify the storyname")]string storyname = "")
        {
            _storyRecorder.CreateNewFile(storyname);
            await ReplyAsync("Create new file " + _storyRecorder.GetCurrentSaveFilename() + " to record the story.");
        }
    }
}
