/**
 * One page of a server-side paged collection.
 *
 * @typeParam T - Element type of the page.
 */
export interface PagedResult<T> {
  /** Elements of the requested page. */
  items: T[];
  /** Total number of elements across all pages. */
  totalCount: number;
  /** One-based index of this page. */
  page: number;
  /** Number of elements requested per page. */
  pageSize: number;
  /** Total number of pages for the current page size. */
  totalPages: number;
  /** Whether a page follows this one. */
  hasNext: boolean;
  /** Whether a page precedes this one. */
  hasPrev: boolean;
}
