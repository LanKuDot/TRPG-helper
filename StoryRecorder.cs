/* Help recording the story and export to an external file.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
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
         * @brief Is the recorder recording?
         */
        private bool _recording = false;
        public bool Recording
        {
            get { return _recording; }
            set {
                _recording = value;
                if (value)  // true
                    ConsoleManager.Message(Discord.LogSeverity.Info, GetType().Name,
                        "Start recording the story");
                else
                    ConsoleManager.Message(Discord.LogSeverity.Info, GetType().Name,
                        "Stop recording the story");
            }
        }
        /*
         * @brief The container that temporarily storing the story.
         */
        private string[] _sentences;
        /*
         * @brief The maximum number of sentences can be stored in _sentences.
         */
        private const int _maxNumOfSentences = 5;
        /*
         * @brief Mark the element of the next slot for saving new sentence in _sentences.
         */
        private int _currentNumOfSentences;
        /*
         * @brief The filename of the output file.
         */
        private string _outputFilename = "";

        /*
         *  @brief Constructor.
         */
        public StoryRecorder()
        {
            _sentences = new string[_maxNumOfSentences];
            _currentNumOfSentences = 0;
        }

        /*
         * @brief Append a new sentence to the container.
         * @param user The information of user
         * @param message The message that is sent by the user.
         * @param isFlushed [out] true If the story in the container is flushed to the file.
         * @return The number of sentences storing in the container.
         * @retval -1 If the Recording flag is false.
         */
        public int AppendNewStory(SocketGuildUser user, string message, out bool isFlushed)
        {
            isFlushed = false;

            if (!_recording)
                return -1;

            /* Filter the command, only recording the dice command. */
            if (message.StartsWith("!") && !message.StartsWith("!dice"))
                return _currentNumOfSentences;

            /* Get the role of the user. */
            IEnumerator<SocketRole> roles = user.Roles.GetEnumerator();
            roles.MoveNext();   // Move to the first item
            roles.MoveNext();   // Ignore "@everyone"

            _sentences[_currentNumOfSentences++] = $"[{roles.Current.Name}] {user.Nickname}: {message}";

            /* Flush the story in the container to the file once the container is full. */
            if (_currentNumOfSentences == _maxNumOfSentences) {
                FlushToFile();
                _currentNumOfSentences = 0;
                isFlushed = true;
            }

            return _currentNumOfSentences;
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

            ConsoleManager.Message(Discord.LogSeverity.Info, GetType().Name,
                $"New file {_outputFilename} is created for recording the story");
        }

        /*
         * @brief Get the filename of the current saving file.
         */
        public string GetCurrentSaveFilename()
        {
            return _outputFilename;
        }

        /*
         * @brief Flush all the sentences in the container to the external file.
         *
         * This method should be called once the container is full.
         * Note that the program will wait until the writing process finishes.
         */
        public void FlushToFile()
        {
            if (_currentNumOfSentences == 0)
                return;

            try {
                using (StreamWriter file = new StreamWriter(Path.Combine(outputDirectory, _outputFilename), true)) {
                    for (int i = 0; i < _currentNumOfSentences; ++i)
                        file.WriteLine(_sentences[i]);
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }

            _currentNumOfSentences = 0;

            ConsoleManager.Message(Discord.LogSeverity.Info, GetType().Name, "Story saved");
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
        [RequireGMPermission]
        [Summary("Create a new file to record the story")]
        public async Task CreateStory([Summary("[Optional] Specify the storyname")]string storyname = "")
        {
            _storyRecorder.CreateNewFile(storyname);
            await ReplyAsync("Create new file " + _storyRecorder.GetCurrentSaveFilename() + " to record the story");
        }

        /*
         * @brief Make StoryRecorder start recording the story.
         *
         * If the output filename is not specified, this method will call CreateStory() first.
         */
        [Command("startRecord")]
        [Alias("記錄故事")]
        [RequireGMPermission]
        [Summary("Start recording the story.")]
        public async Task StartRecordStory()
        {
            _storyRecorder.Recording = true;

            // If the filename is not specified, initialize it.
            if (string.IsNullOrEmpty(_storyRecorder.GetCurrentSaveFilename()))
                await CreateStory();

            await ReplyAsync("Start recording the story.");
        }

        /*
         * @brief Make StoryRecorder stop recording the story.
         */
        [Command("stopRecord")]
        [Alias("停止記錄")]
        [RequireGMPermission]
        [Summary("Stop recording the story.")]
        public async Task StopRecordStory()
        {
            _storyRecorder.Recording = false;
            _storyRecorder.FlushToFile();

            await ReplyAsync("Stop recording the story. Story saved.");
        }

        /*
         * @brief List the latest n record files.
         * @param listNum Specify the latest n files to be listed.
         */
        [Command("listRecord")]
        [Alias("列出記錄")]
        [RequireGMPermission]
        [Summary("List the latest record files")]
        public async Task ListStoryFiles([Summary("[Optional] Specify the latest n files to show.")]int listNum = 5)
        {
            // If the output directory is not existing, quit.
            if (!Directory.Exists(StoryRecorder.outputDirectory)) {
                await ReplyAsync("There has no record yet.");
                return;
            }

            /* List files */
            string[] recordFiles = Directory.GetFiles(StoryRecorder.outputDirectory, "*.txt", SearchOption.TopDirectoryOnly);
            if (recordFiles.Length == 0)
                await ReplyAsync("There has no record yet.");
            else {
                string replyString = "";
                int i;

                for (i = recordFiles.Length - 1; listNum != 0 && i >= 0; --i, --listNum)
                    replyString += Path.GetFileName(recordFiles[i]) + "\n";

                if (i < 0)
                    replyString += "--END--";
                else
                    replyString += $"--{i+1} MORE--";

                await ReplyAsync(replyString);
            }
        }
    }
}
