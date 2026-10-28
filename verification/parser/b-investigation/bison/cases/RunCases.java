import java.io.IOException;
import java.util.ArrayList;
public class RunCases {
  static class Input implements BaseParser.Lexer, VariantParser.Lexer, PrunedParser.Lexer, WitnessParser.Lexer, VariantReverseParser.Lexer, WitnessReverseParser.Lexer {
    final String source; int offset; final ArrayList<String> errors = new ArrayList<>();
    Input(String source) { this.source=source; }
    public Object getLVal() { return null; }
    public int yylex() { return offset == source.length() ? 0 : source.charAt(offset++); }
    public void yyerror(String msg) { errors.add(msg); }
  }
  public static void main(String[] args) throws IOException {
    for (String input : new String[] { "z", "ax", "", "a", "axx", "axu" }) {
      Input base=new Input(input); System.out.println("base|"+input+"|"+new BaseParser(base).parse()+"|"+base.errors);
      Input variant=new Input(input); System.out.println("variant|"+input+"|"+new VariantParser(variant).parse()+"|"+variant.errors);
      Input pruned=new Input(input); System.out.println("pruned|"+input+"|"+new PrunedParser(pruned).parse()+"|"+pruned.errors);
      Input variantReverse=new Input(input); System.out.println("variant-reverse|"+input+"|"+new VariantReverseParser(variantReverse).parse()+"|"+variantReverse.errors);
      Input witnessReverse=new Input(input); System.out.println("witness-reverse|"+input+"|"+new WitnessReverseParser(witnessReverse).parse()+"|"+witnessReverse.errors);
      Input witness=new Input(input); System.out.println("witness|"+input+"|"+new WitnessParser(witness).parse()+"|"+witness.errors);
    }
  }
}

