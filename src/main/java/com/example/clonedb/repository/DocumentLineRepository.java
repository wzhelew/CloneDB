package com.example.clonedb.repository;

import com.example.clonedb.model.DocumentHeader;
import com.example.clonedb.model.DocumentLine;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;

public interface DocumentLineRepository extends JpaRepository<DocumentLine, Long> {
    List<DocumentLine> findByDocument(DocumentHeader document);
}
