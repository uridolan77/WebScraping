using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScraperApi.Data.Entities
{
    public class CustomMetricEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("scraperMetricsId")]
        public int ScraperMetricsId { get; set; }

        [Required]
        [Column("metricName")]
        public string MetricName { get; set; }

        [Required]
        [Column("metricValue")]
        public double MetricValue { get; set; }
    }
}
