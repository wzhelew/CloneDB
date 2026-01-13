package com.example.clonedb.service;

import com.example.clonedb.dto.DocumentDetailsDto;
import com.example.clonedb.dto.DocumentLineDto;
import com.example.clonedb.dto.DocumentSummaryDto;
import com.example.clonedb.model.DocumentHeader;
import com.example.clonedb.model.DocumentLine;
import com.example.clonedb.repository.DocumentHeaderRepository;
import com.example.clonedb.repository.DocumentLineRepository;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.LocalDate;
import java.util.List;
import java.util.stream.Collectors;

@Service
public class DocumentService {
    private final DocumentHeaderRepository headerRepository;
    private final DocumentLineRepository lineRepository;

    public DocumentService(DocumentHeaderRepository headerRepository, DocumentLineRepository lineRepository) {
        this.headerRepository = headerRepository;
        this.lineRepository = lineRepository;
    }

    @Transactional(readOnly = true)
    public List<DocumentSummaryDto> getDocumentsByDateRange(LocalDate from, LocalDate to) {
        return headerRepository.findByDocumentDateBetween(from, to).stream()
                .map(doc -> new DocumentSummaryDto(
                        doc.getId(),
                        doc.getCompany().getCompanyName(),
                        doc.getSupplier().getName(),
                        doc.getDocumentDate()
                ))
                .collect(Collectors.toList());
    }

    @Transactional(readOnly = true)
    public DocumentDetailsDto getDocumentDetails(Long documentId) {
        DocumentHeader document = headerRepository.findById(documentId)
                .orElseThrow(() -> new IllegalArgumentException("Document not found"));

        List<DocumentLineDto> lines = lineRepository.findByDocument(document).stream()
                .map(this::mapLine)
                .collect(Collectors.toList());

        return new DocumentDetailsDto(
                document.getId(),
                document.getCompany().getCompanyName(),
                document.getSupplier().getName(),
                document.getDocumentDate(),
                lines
        );
    }

    private DocumentLineDto mapLine(DocumentLine line) {
        return new DocumentLineDto(
                line.getItem().getItemCode(),
                line.getItem().getItemName(),
                line.getItem().getBarcode(),
                line.getQuantity(),
                line.getPrice()
        );
    }
}
