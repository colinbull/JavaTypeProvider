package hello;

public class HelloWorld {
    private String name;

    public HelloWorld() {
        this.name = "World";
    }

    public HelloWorld(String name) {
        this.name = name;
    }

    @Override
    public String toString() {
        return "Hello " + name;
    }
}
