package com.example.clonedb.dto;

import java.time.LocalDate;

public class DocumentSummaryDto {
    private final Long id;
    private final String companyName;
    private final String supplierName;
    private final LocalDate documentDate;

    public DocumentSummaryDto(Long id, String companyName, String supplierName, LocalDate documentDate) {
        this.id = id;
        this.companyName = companyName;
        this.supplierName = supplierName;
        this.documentDate = documentDate;
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
}
