package com.example.clonedb.repository;

import com.example.clonedb.model.DocumentHeader;
import org.springframework.data.jpa.repository.JpaRepository;

import java.time.LocalDate;
import java.util.List;

public interface DocumentHeaderRepository extends JpaRepository<DocumentHeader, Long> {
    List<DocumentHeader> findByDocumentDateBetween(LocalDate from, LocalDate to);
}
