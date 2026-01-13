package com.example.clonedb.model;

import jakarta.persistence.*;

@Entity
@Table(name = "document_lines")
public class DocumentLine {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @ManyToOne(optional = false)
    private DocumentHeader document;

    @ManyToOne(optional = false)
    private Item item;

    @Column(nullable = false)
    private Integer quantity;

    @Column(nullable = false)
    private Double price;

    public DocumentLine() {
    }

    public DocumentLine(DocumentHeader document, Item item, Integer quantity, Double price) {
        this.document = document;
        this.item = item;
        this.quantity = quantity;
        this.price = price;
    }

    public Long getId() {
        return id;
    }

    public DocumentHeader getDocument() {
        return document;
    }

    public void setDocument(DocumentHeader document) {
        this.document = document;
    }

    public Item getItem() {
        return item;
    }

    public void setItem(Item item) {
        this.item = item;
    }

    public Integer getQuantity() {
        return quantity;
    }

    public void setQuantity(Integer quantity) {
        this.quantity = quantity;
    }

    public Double getPrice() {
        return price;
    }

    public void setPrice(Double price) {
        this.price = price;
    }
}
