package com.example.clonedb.dto;

import java.time.LocalDate;
import java.util.List;

public class DocumentDetailsDto {
    private final Long id;
    private final String companyName;
    private final String supplierName;
    private final LocalDate documentDate;
    private final List<DocumentLineDto> lines;

    public DocumentDetailsDto(Long id, String companyName, String supplierName, LocalDate documentDate, List<DocumentLineDto> lines) {
        this.id = id;
        this.companyName = companyName;
        this.supplierName = supplierName;
        this.documentDate = documentDate;
        this.lines = lines;
    }

    public Long getId() {
        return id;
    }

    public String getCompanyName() {
        return companyName;
    }

    public String getSupplierName() {
        return supplierName;
    }

    public LocalDate getDocumentDate() {
        return documentDate;
    }

    public List<DocumentLineDto> getLines() {
        return lines;
    }
}
