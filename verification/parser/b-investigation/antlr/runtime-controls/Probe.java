import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.misc.ParseCancellationException;
import java.lang.reflect.InvocationTargetException;
import java.util.*;

public final class Probe {
  public static void main(String[] args) throws Exception {
    for (String name : new String[]{"Pruned", "RightDead"}) {
      for (String input : new String[]{"ax", "a", "axq", ""}) {
        List<String> errors = new ArrayList<>();
        BaseErrorListener listener = new BaseErrorListener() {
          @Override public void syntaxError(Recognizer<?,?> recognizer, Object symbol, int line, int pos, String message, RecognitionException error) { errors.add(message); }
        };
        Lexer lexer = (Lexer)Class.forName(name + "Lexer").getConstructor(CharStream.class).newInstance(CharStreams.fromString(input));
        lexer.removeErrorListeners(); lexer.addErrorListener(listener);
        CommonTokenStream tokens = new CommonTokenStream(lexer);
        Parser parser = (Parser)Class.forName(name + "Parser").getConstructor(TokenStream.class).newInstance(tokens);
        parser.removeErrorListeners(); parser.addErrorListener(listener); parser.setErrorHandler(new BailErrorStrategy());
        boolean accepted = false;
        String tree = "";
        try {
          ParserRuleContext root = (ParserRuleContext)parser.getClass().getMethod("start").invoke(parser);
          accepted = errors.isEmpty() && parser.getNumberOfSyntaxErrors() == 0 && tokens.LA(1) == Token.EOF;
          tree = root.toStringTree(parser);
        } catch (InvocationTargetException e) {
          errors.add(e.getCause().getClass().getSimpleName());
        }
        System.out.println(name + "\tinput=" + input + "\taccepted=" + accepted + "\terrors=" + errors + "\ttree=" + tree);
      }
    }
  }
}
