package com.example;

/**
 * Created with IntelliJ IDEA.
 * User: Colin
 * Date: 02/03/13
 * Time: 17:25
 * To change this template use File | Settings | File Templates.
 */
public class Calculator {

    public CalculationResult compute(String expression){
           return new CalculationResult(expression, 10.);
    }
}
