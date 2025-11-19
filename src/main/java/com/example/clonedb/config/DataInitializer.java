package com.example.clonedb.config;

import com.example.clonedb.model.*;
import com.example.clonedb.repository.*;
import org.springframework.boot.CommandLineRunner;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

import java.time.LocalDate;

@Configuration
public class DataInitializer {
    @Bean
    CommandLineRunner seedDatabase(CompanyRepository companyRepository,
                                   SupplierRepository supplierRepository,
                                   DocumentHeaderRepository documentHeaderRepository,
                                   ItemRepository itemRepository,
                                   DocumentLineRepository documentLineRepository) {
        return args -> {
            if (companyRepository.count() > 0) {
                return;
            }

            Company company = companyRepository.save(new Company("Demo Company", "demo", "password"));
            Supplier supplierA = supplierRepository.save(new Supplier("Supplier A"));
            Supplier supplierB = supplierRepository.save(new Supplier("Supplier B"));

            DocumentHeader doc1 = documentHeaderRepository.save(new DocumentHeader(company, supplierA, LocalDate.now().minusDays(2)));
            DocumentHeader doc2 = documentHeaderRepository.save(new DocumentHeader(company, supplierB, LocalDate.now().minusDays(1)));

            Item flour = itemRepository.save(new Item("1234567890123", "ITM-001", "Flour", "B-100"));
            Item sugar = itemRepository.save(new Item("2345678901234", "ITM-002", "Sugar", "B-101"));
            Item salt = itemRepository.save(new Item("3456789012345", "ITM-003", "Salt", "B-102"));

            documentLineRepository.save(new DocumentLine(doc1, flour, 10, 1.20));
            documentLineRepository.save(new DocumentLine(doc1, sugar, 5, 2.40));
            documentLineRepository.save(new DocumentLine(doc2, salt, 15, 0.80));
        };
    }
}
