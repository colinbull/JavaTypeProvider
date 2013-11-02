package com.example;

/**
 * Created with IntelliJ IDEA.
 * User: Colin
 * Date: 02/03/13
 * Time: 17:26
 * To change this template use File | Settings | File Templates.
 */
public class CalculationResult {

    private String expr;
    private Double result;

    public CalculationResult(String expr, Double result){
        this.expr = expr;
        this.result = result;
    }

    public String getOriginalExpression(){
        return expr;
    }

    public Double getResult() {
        return result;
    }
}
