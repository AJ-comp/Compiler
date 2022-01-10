namespace CommandPrompt.Builder
{
    public class ExceptionCode
    {
        /// <summary>
        /// 디렉토리가 이미 존재합니다.
        /// </summary>
        public static int AlreadyExistDirectory = 0x20000000;
        /// <summary>
        /// 파일의 확장자가 올바르지 않습니다.
        /// </summary>
        public static int NotCorrectFileExtension = 0x20000001;
        /// <summary>
        /// 절대 경로여야 합니다.
        /// </summary>
        public static int MustBeAbsolutePath = 0x20000002;
    }
}
