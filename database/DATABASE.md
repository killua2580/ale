# Supabase Database Analysis & Schema Summary

## Overview
This document provides an up-to-date analysis of your active Supabase database (project: "molka"). It details the tables, columns, and relationships, reflecting the current live schema.

---

## Main Tables & Their Structure

### 1. students
- **id** (uuid, PK, auto-generated)
- **email** (text, unique, required)
- **cin** (text, unique, required)
- **name** (text, optional)

**Relationships:**
- Referenced by: book_dislikes, borrowed_books, reserved_books, book_ratings, book_favorites (as student_id)

---

### 2. admins
- **id** (uuid, PK, auto-generated)
- **email** (text, unique, required)
- **name** (text, optional)

---

### 3. books
- **id** (uuid, PK, auto-generated)
- **bibid** (text, unique, required)
- **title** (text, required)
- **author** (text, required)
- **description** (text, optional)
- **cover_image_url** (text, optional)
- **is_available** (boolean, default true)

**Relationships:**
- Referenced by: book_dislikes, book_favorites, book_categories, borrowed_books, reserved_books, book_ratings (as book_id)

---

### 4. categories
- **id** (uuid, PK, auto-generated)
- **name** (text, unique, required)

**Relationships:**
- Referenced by: book_categories (as category_id)

---

### 5. book_categories (join table)
- **book_id** (uuid, FK to books)
- **category_id** (uuid, FK to categories)

---

### 6. borrowed_books
- **id** (uuid, PK, auto-generated)
- **student_id** (uuid, FK to students)
- **book_id** (uuid, FK to books)
- **borrow_date** (date, default current date)
- **return_due_date** (date, optional)
- **returned** (boolean, default false)

---

### 7. reserved_books
- **id** (uuid, PK, auto-generated)
- **student_id** (uuid, FK to students)
- **book_id** (uuid, FK to books)
- **reserved_date** (date, default current date)

---

### 8. book_ratings
- **id** (uuid, PK, auto-generated)
- **student_id** (uuid, FK to students)
- **book_id** (uuid, FK to books)
- **rating** (integer, 0-5)
- **rated_at** (timestamp, default current timestamp)

---

### 9. book_favorites
- **id** (uuid, PK, auto-generated)
- **student_id** (uuid, FK to students)
- **book_id** (uuid, FK to books)
- **favorited_at** (timestamp, default current timestamp)

---

### 10. book_dislikes
- **id** (uuid, PK, auto-generated)
- **student_id** (uuid, FK to students)
- **book_id** (uuid, FK to books)
- **disliked_at** (timestamp, default current timestamp)

---

## Relationships

- **students** can have many borrowed_books, reserved_books, book_ratings, book_favorites, book_dislikes.
- **books** can have many borrowed_books, reserved_books, book_ratings, book_favorites, book_dislikes, and belong to many categories via book_categories.
- **categories** can have many books via book_categories.
- **admins** is a separate table for admin users.

---

## Observations

- The schema is normalized and uses UUIDs for all primary keys.
- There are join tables for many-to-many relationships (e.g., book_categories).
- All foreign key relationships are explicit and enforce referential integrity.
- There are tables for tracking user interactions with books: favorites, dislikes, ratings, borrows, and reservations.
- The schema supports both students and admins, but keeps them in separate tables.

---

## Conclusion

This schema is robust for a library management system, supporting user management, book inventory, borrowing, reservations, ratings, likes/dislikes, and categorization. It is well-structured for scalability and data integrity.
