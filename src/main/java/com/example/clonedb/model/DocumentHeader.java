package com.example.clonedb.model;

import jakarta.persistence.*;

import java.time.LocalDate;

@Entity
@Table(name = "documents")
public class DocumentHeader {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @ManyToOne(optional = false)
    private Company company;

    @ManyToOne(optional = false)
    private Supplier supplier;

    @Column(name = "document_date", nullable = false)
    private LocalDate documentDate;

    public DocumentHeader() {
    }

    public DocumentHeader(Company company, Supplier supplier, LocalDate documentDate) {
        this.company = company;
        this.supplier = supplier;
        this.documentDate = documentDate;
    }

    public Long getId() {
        return id;
    }

    public Company getCompany() {
        return company;
    }

    public void setCompany(Company company) {
        this.company = company;
    }

    public Supplier getSupplier() {
        return supplier;
    }

    public void setSupplier(Supplier supplier) {
        this.supplier = supplier;
    }

    public LocalDate getDocumentDate() {
        return documentDate;
    }

    public void setDocumentDate(LocalDate documentDate) {
        this.documentDate = documentDate;
    }
}
