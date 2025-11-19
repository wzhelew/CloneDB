package com.example.clonedb.dto;

public class DocumentLineDto {
    private final String itemCode;
    private final String itemName;
    private final String barcode;
    private final Integer quantity;
    private final Double price;

    public DocumentLineDto(String itemCode, String itemName, String barcode, Integer quantity, Double price) {
        this.itemCode = itemCode;
        this.itemName = itemName;
        this.barcode = barcode;
        this.quantity = quantity;
        this.price = price;
    }

    public String getItemCode() {
        return itemCode;
    }

    public String getItemName() {
        return itemName;
    }

    public String getBarcode() {
        return barcode;
    }

    public Integer getQuantity() {
        return quantity;
    }

    public Double getPrice() {
        return price;
    }
}
