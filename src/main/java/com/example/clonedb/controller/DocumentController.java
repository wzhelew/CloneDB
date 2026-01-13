package com.example.clonedb.controller;

import com.example.clonedb.dto.DocumentDetailsDto;
import com.example.clonedb.dto.DocumentSummaryDto;
import com.example.clonedb.service.DocumentService;
import org.springframework.format.annotation.DateTimeFormat;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.time.LocalDate;
import java.util.List;

@RestController
@RequestMapping("/documents")
public class DocumentController {
    private final DocumentService documentService;

    public DocumentController(DocumentService documentService) {
        this.documentService = documentService;
    }

    @GetMapping
    public ResponseEntity<List<DocumentSummaryDto>> getDocuments(
            @RequestParam("from") @DateTimeFormat(iso = DateTimeFormat.ISO.DATE) LocalDate from,
            @RequestParam("to") @DateTimeFormat(iso = DateTimeFormat.ISO.DATE) LocalDate to) {
        return ResponseEntity.ok(documentService.getDocumentsByDateRange(from, to));
    }

    @GetMapping("/{id}")
    public ResponseEntity<DocumentDetailsDto> getDocument(@PathVariable("id") Long id) {
        return ResponseEntity.ok(documentService.getDocumentDetails(id));
    }
}
